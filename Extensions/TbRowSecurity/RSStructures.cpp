#include "stdafx.h" 

#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\Sqltable.h>
#include "TBRowSecurityEnums.h"
#include "RSManager.h"
#include "RSTables.h"
#include "RSStructures.h"

///////////////////////////////////////////////////////////////////////////////
//						RSEntityInfo declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
void RSEntityInfo::Assign(RSEntityInfo* pEntity)
{
	m_strName					= pEntity->m_strName;
	m_strTitle					= pEntity->m_strTitle;
	m_strDescription			= pEntity->m_strDescription;
	m_strMasterTable			= pEntity->m_strMasterTable;
	m_strAutonumberNamespace	= pEntity->m_strAutonumberNamespace;
	m_MasterTableNamespace		= pEntity->m_MasterTableNamespace;

	for (int r = 0; r < pEntity->m_arrDocNamespace.GetSize(); r++)
		m_arrDocNamespace.Add(pEntity->m_arrDocNamespace.GetAt(r));
	
	m_arColumns.RemoveAll();

	for (int i = 0; i <= pEntity->m_arColumns.GetUpperBound(); i++)
		m_arColumns.Add(pEntity->m_arColumns.GetAt(i));
}

//-----------------------------------------------------------------------------
BOOL RSEntityInfo::Parse(CXMLNode* pnEntity)
{
	CXMLNode* pnColumn = NULL;
	CXMLNode* pnColumsNode = NULL;
	CXMLNodeChildsList* pnColumns = NULL;
	CString colName;
					
	// Entity node
	CString strNamespace;
	pnEntity->GetAttribute(_T("name"), m_strName);

	CTBNamespace autoNamespace;
	//behaviour per la parte di autonumerazione del campo RowSecurityID
	autoNamespace.AutoCompleteNamespace(CTBNamespace::ENTITY, _T("Extensions.TBRowSecurity.TBRowSecurity"), autoNamespace);
	autoNamespace.SetObjectName(cwsprintf(_T("RS%s"), m_strName));
	m_strAutonumberNamespace = autoNamespace.ToString();

	// MasterTableNamespace node
	CString strMasterTblNs;
	CXMLNode* pnMasterTblNs = pnEntity->GetChildByName(_T("MasterTableNamespace"));
	if (pnMasterTblNs)
		pnMasterTblNs->GetText(strMasterTblNs);
	m_MasterTableNamespace.AutoCompleteNamespace(CTBNamespace::TABLE, strMasterTblNs, m_MasterTableNamespace);
	ASSERT(m_MasterTableNamespace.IsValid());
	
	m_strMasterTable = m_MasterTableNamespace.GetObjectName(); // estrapolo il solo nome della tabella master
	
	// DocumentNamespace node
	CString strDocNs;
	CXMLNode* pnDocNss = pnEntity->GetChildByName(_T("DocumentNamespaces"));
	CXMLNode* pChildNS;
	CXMLNodeChildsList* pnChilds = NULL;
	CTBNamespace m_DocNamespace;
	if (!pnDocNss)
		return FALSE;

	if (pnChilds = pnDocNss->GetChilds())
	{
		for (int i = 0; i < pnChilds->GetCount(); i++)
		{
			pChildNS = pnChilds->GetAt(i);
			if (pChildNS)
			{
				if (!pChildNS->GetAttribute(_T("ns"), strDocNs))
					pChildNS->GetText(strDocNs);
				
				m_DocNamespace.AutoCompleteNamespace(CTBNamespace::DOCUMENT, strDocNs, m_DocNamespace);
				m_arrDocNamespace.Add(m_DocNamespace);
			}
		}
	}

 
	// Description node
	CXMLNode* pnDescri = pnEntity->GetChildByName(_T("Description"));
	if (pnDescri)
		pnDescri->GetText(m_strDescription);

	if (m_strName.IsEmpty() || m_strMasterTable.IsEmpty())
		return FALSE;
					
	if (pnColumsNode = pnEntity->GetChildByName(_T("RSColumns")))
	{
		if (pnColumns = pnColumsNode->GetChilds())
		{
			for (int j=0; j < pnColumns->GetCount(); j++)
			{
				pnColumn = pnColumns->GetAt(j);
				if (pnColumn)
				{
					pnColumn->GetText(colName);
					m_arColumns.Add(colName);
				}
			}
		}
		return TRUE;
	}
	return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//						RSSingleColumn declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
BOOL RSSingleColumn::Parse(CXMLNode* pnColumn)
{
	pnColumn->GetText(m_strTableColumn);
	pnColumn->GetAttribute(_T("entitycolumn"), m_strEntityColumn);
	return !m_strTableColumn.IsEmpty() && !m_strEntityColumn.IsEmpty();
}



///////////////////////////////////////////////////////////////////////////////
//						RSProtectedColumns declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
BOOL RSProtectedColumns::Parse(CXMLNode* pnColumns)
{
	CXMLNode* pnColumn = NULL;
	CXMLNodeChildsList* pnColumnsNode = NULL;

	if (pnColumnsNode = pnColumns->GetChilds())
	{
		for (int i = 0; i < pnColumnsNode->GetCount(); i++)
		{
			pnColumn = pnColumnsNode->GetAt(i);
			if (pnColumn)
			{
				RSSingleColumn* pProtectedCol = new RSSingleColumn();
				if (pProtectedCol->Parse(pnColumn))
					m_arProtectedColumns.Add(pProtectedCol);
				else
				{
					delete pProtectedCol;
					return FALSE;
				}
			}
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
RSProtectedColumns::~RSProtectedColumns()
{
	for (int i = 0; i <= m_arProtectedColumns.GetUpperBound(); i++)
	{
		delete m_arProtectedColumns.GetAt(i);
		m_arProtectedColumns.SetAt(i, NULL);
	}
	m_arProtectedColumns.RemoveAll();
}


///////////////////////////////////////////////////////////////////////////////
//						RSEntityTableInfo declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RSEntityTableInfo::RSEntityTableInfo()
:
	m_pEntityInfo(NULL)
{
}

//-----------------------------------------------------------------------------
RSEntityTableInfo::~RSEntityTableInfo()
{
	m_pEntityInfo = NULL;
	for (int i = m_arAllProtectedColumns.GetUpperBound(); i>= 0; i--)
	{
		delete m_arAllProtectedColumns.GetAt(i);
		//m_arAllProtectedColumns.SetAt(i, NULL);
	}
	m_arAllProtectedColumns.RemoveAll();
}

//-----------------------------------------------------------------------------
BOOL RSEntityTableInfo::Parse(CXMLNode* pnEntity)
{
	CString strName;
	CXMLNode* pnColumn = NULL;	
	CXMLNodeChildsList* pnColumnsNode = NULL;

	pnEntity->GetAttribute(_T("name"), strName);
	if (strName.IsEmpty())
		return FALSE;
				
	m_pEntityInfo = AfxGetRowSecurityManager()->GetEntityInfo(strName, TRUE);

	if (pnColumnsNode = pnEntity->GetChilds())
	{
		for (int i=0; i < pnColumnsNode->GetCount(); i++)
		{
			pnColumn = pnColumnsNode->GetAt(i);
			if (pnColumn)
			{
				RSProtectedColumns* pProtectedCol = new RSProtectedColumns();
				if (pProtectedCol->Parse(pnColumn))
					m_arAllProtectedColumns.Add(pProtectedCol);
				else
				{
					delete pProtectedCol;
					return FALSE;
				}
			}
		}
	}

	return TRUE;
}

//esempio di filtraggio sulla tabella OM_Task che contiene riferimenti a due entità: CLIENT e OFFICEFILE
/*
SELECT TaskId,MasterCode,ReferenceCode,OfficeFileId,LinkedDlGroupId,WorkerId,TaskTypeId,Description,InternalNotes,TaskDate,StartTime,EndTime,Expenses,FixedExpenses,TaxesExpenses,InAdvanceExpenses,HourlyPrice,FixedPrice,TotalPrice,IsConfirmed,IsSuspended,IsStillToLink,ValuationId,IsZeroPrice,AutomaticGenerated,Length,Quantity,TaskTarget,Origin,TaxesExpensesDescription,InAdvanceExpensesDescription,DirectCostWorker,IndirectCostWorker,HourlyPriceTarget,FixedPriceTarget,HourlyPriceCalc,FixedPriceCalc,TotalPriceCalc,TBGuid,TBCreated,TBModified,TBCreatedID,TBModifiedID 
  FROM OM_Tasks 
  WHERE (OM_Tasks.MasterCode = '' OR EXISTS (SELECT * FROM OM_Masters LEFT OUTER JOIN RS_SubjectsGrants ON OM_Masters.RowSecurityID = RS_SubjectsGrants.RowSecurityID 
	WHERE (OM_Masters.MasterCode = OM_Tasks.MasterCode AND (OM_Masters.IsProtected = '0' OR (RS_SubjectsGrants.EntityName = 'CLIENT'  AND RS_SubjectsGrants.WorkerID = 9))))) 
  AND  (OM_Tasks.OfficeFileId = 0 OR EXISTS 
		(SELECT * FROM OM_OfficeFiles LEFT OUTER JOIN RS_SubjectsGrants ON OM_OfficeFiles.RowSecurityID = RS_SubjectsGrants.RowSecurityID 
				 WHERE (OM_OfficeFiles.OfficeFileId = OM_Tasks.OfficeFileId AND (OM_OfficeFiles.IsProtected = '0' OR (RS_SubjectsGrants.EntityName = 'OFFICEFILE'  AND RS_SubjectsGrants.WorkerID = 9)))))
*/

//quando la protezione si deve applicare a due campi distinti 
//SELECT CustSuppType,Customer,Category,CommissionCtg,Area,Salesperson,AreaManager,IsAPrivatePerson,SuspendedTax,ExemptFromTax,TaxCode,GoodsOffset,ServicesOffset,Blocked,OpenedAdmCases,OpenedAdmCasesAmount,DebitFreeSamplesTaxAmount,Port,Carrier1,Carrier2,Carrier3,LastDocNo,LastDocDate,LastDocTotal,LastPaymentTerm,DeclarationOfIntentNo,DeclarationOfIntentDate,DeclarationOfIntentOurNo,InvoicingGroup,FreeOfChargeLevel,CashOnDeliveryLevel,NoCarrierCharges,PackCharges,ShippingCharges,ChargesPercOnTotAmt,ShowPricesOnDN,OneInvoicePerDN,OneReturnFromCustomerPerCN,GroupItems,GroupBills,Package,Transport,ReferencesPrintType,CashOrderCharges,DebitStampCharges,DebitCollectionCharges,GroupOrders,LotSelection,LotOverbook,GroupCostAccounting,ReqForPymtThreshold,ReqForPymtLastLevel,ReqForPymtLastDate,NoOfMaxLevelReqForPymt,UseReqForPymt,NoPrintDueDate,InvoicingCustomer,Priority,Variant,OneInvoicePerOrder,OneDNPerOrder,Shipping,PenalityPerc,WithholdingTaxManagement,WithholdingTaxPerc,WithholdingTaxBasePerc,ExcludedFromWEEE,DeclarationOfIntentDueDate,OneDocumentPerPL,AllocationArea,DirectAllocation,CustomerClassification,CustomerSpecification,DeclarationOfIntentId,CrossDocking,ConsignmentPartner,PASplitPayment,PublicAuthority,MaxOrderValue,MaxOrderedValue,MaximumCredit,MaxOrderValueCheckType,MaxOrderedValueCheckType,MaximumCreditCheckType,MaxOrderValueDate,MaxOrderedValueDate,MaximumCreditDate,TBCreated,TBModified,TBCreatedID,TBModifiedID
//
//FROM MA_CustSuppCustomerOptions WHERE CustSuppType = 3211264 AND Customer = '0053' AND
//
//)(( ISNULL(MA_CustSuppCustomerOptions.Salesperson, '') = '' AND ( ISNULL(MA_CustSuppCustomerOptions.AreaManager, '') = '' )
//
//OR
//(
//EXISTS (SELECT * FROM MA_SalesPeople LEFT OUTER JOIN RS_SubjectsGrants ON MA_SalesPeople.RowSecurityID = RS_SubjectsGrants.RowSecurityID
//WHERE
//(MA_SalesPeople.Salesperson = MA_CustSuppCustomerOptions.Salesperson AND (MA_SalesPeople.IsProtected = '0' OR (RS_SubjectsGrants.EntityName = 'SALEPEOPLE'  AND RS_SubjectsGrants.WorkerID = 47)))))
//OR
//EXISTS (SELECT * FROM MA_SalesPeople LEFT OUTER JOIN RS_SubjectsGrants ON MA_SalesPeople.RowSecurityID = RS_SubjectsGrants.RowSecurityID
//WHERE
//(MA_SalesPeople.Salesperson = MA_CustSuppCustomerOptions.AreaManager AND (MA_SalesPeople.IsProtected = '0' OR (RS_SubjectsGrants.EntityName = 'SALEPEOPLE'  AND RS_SubjectsGrants.WorkerID = 47)))))
//))

//-----------------------------------------------------------------------------
CString RSEntityTableInfo::GetFilterText(SqlTable* pTable, SqlTableItem* pTableItem)
{
	if (!pTable || !pTableItem->m_pRecord)
		return _T("");

	
	RSProtectedColumns* pRSColumns = NULL;
	RSSingleColumn* pRSSingleColumn = NULL;
	DataObj* pRSWorker = pTable->GetRowSecurityFilterWorker();
	//se il programmatore ha abilitato la possibilità di cambiare il worker allora aggiungo il parametro altrimenti no
	CString strCurrWorkerID = (pRSWorker) ? _T("?") : pTable->m_pSqlConnection->NativeConvert(&DataLng(AfxGetWorkerId()));

	CString strFilter;
		
	CString currTableName = (pTableItem->m_strAliasName.IsEmpty()) ? pTableItem->m_strTableName : pTableItem->m_strAliasName;
	//caso1
	if (*pTableItem->m_pRecord->GetNamespace() == m_pEntityInfo->m_MasterTableNamespace)
		strFilter = cwsprintf(_T(" (ISNULL(%s.IsProtected, 0) = %s OR EXISTS (SELECT RowSecurityID FROM RS_SubjectsGrants WHERE EntityName = %s AND WorkerID = %s AND RS_SubjectsGrants.RowSecurityID = %s.RowSecurityID))"),
			currTableName, pTable->m_pSqlConnection->NativeConvert(&DataBool(FALSE)), pTable->m_pSqlConnection->NativeConvert(&DataStr(m_pEntityInfo->m_strName)), strCurrWorkerID, currTableName);
	else			
	{//caso 2
		
		CString firstColumn;
		CString firstMasterColumn;		
		CString joinTxt;
		CString nullFilter;
		CString existFilter;
	
		//scorro gli elementi presenti nell'array m_arAllProtectedColumns
		
		//considero le colonne che mi consentono di fare la join con la tabella master. La join la devo inserire nella subquery
		for (int i = 0; i < m_arAllProtectedColumns.GetSize(); i++)
		{
			pRSColumns = m_arAllProtectedColumns.GetAt(i);
			joinTxt.Empty();
			CString singleNullFilter;
			// devo scorrere i vari segmenti che mi permettono di accedere alla tabella master
			for (int j = 0; j < pRSColumns->m_arProtectedColumns.GetSize(); j++)
			{
				pRSSingleColumn = pRSColumns->m_arProtectedColumns.GetAt(j);

				DataObj* pDataObj = pTableItem->m_pRecord->GetDataObjFromColumnName(pRSSingleColumn->m_strTableColumn);
				if (pDataObj && pDataObj->GetDataType().m_wType != DATA_BOOL_TYPE && pDataObj->GetDataType().m_wType != DATA_ENUM_TYPE)
				{
					CString strEmptyValue = _T("\'\'");				
					DataObj* pCloneDataObj = pDataObj->Clone();
					pCloneDataObj->Clear();
					strEmptyValue = pTable->m_pSqlConnection->NativeConvert(pCloneDataObj);
					delete pCloneDataObj;
					//(( ISNULL(MA_CustSuppCustomerOptions.Salesperson, '') = '' AND ( ISNULL(MA_CustSuppCustomerOptions.AreaManager, '') = '' )
					if (!singleNullFilter.IsEmpty())
						singleNullFilter += _T(" OR ");
					singleNullFilter += cwsprintf(_T(" ( ISNULL(%s.%s, %s) = %s )"), currTableName, pRSSingleColumn->m_strTableColumn, strEmptyValue, strEmptyValue);
				}
				else
				{
					//il datoobj non è presente nel SqlRecord: WOORM crea un SqlRecord al volo con i soli campi selezionati nella quary
					//onde evitare di istanziare il SqlRecord chiedo al SqlCatalog il tipo del campo
				}

				if (!joinTxt.IsEmpty())
					joinTxt += _T(" AND ");
				joinTxt += cwsprintf(_T("%s.%s = %s.%s"), m_pEntityInfo->m_strMasterTable, pRSSingleColumn->m_strEntityColumn, currTableName, pRSSingleColumn->m_strTableColumn);					
			}
			//OR
			//(
			//EXISTS (SELECT * FROM MA_SalesPeople LEFT OUTER JOIN RS_SubjectsGrants ON MA_SalesPeople.RowSecurityID = RS_SubjectsGrants.RowSecurityID
			//WHERE
			//(MA_SalesPeople.Salesperson = MA_CustSuppCustomerOptions.Salesperson AND (MA_SalesPeople.IsProtected = '0' OR (RS_SubjectsGrants.EntityName = 'SALEPEOPLE'  AND RS_SubjectsGrants.WorkerID = 47)))))
			existFilter += cwsprintf(_T(" OR ( EXISTS (SELECT * FROM %s LEFT OUTER JOIN RS_SubjectsGrants ON %s.RowSecurityID = RS_SubjectsGrants.RowSecurityID WHERE (%s AND (%s.IsProtected = %s OR (RS_SubjectsGrants.EntityName = %s  AND RS_SubjectsGrants.WorkerID = %s)))))"),
					m_pEntityInfo->m_strMasterTable, m_pEntityInfo->m_strMasterTable, joinTxt, m_pEntityInfo->m_strMasterTable, pTable->m_pSqlConnection->NativeConvert(&DataBool(FALSE)), pTable->m_pSqlConnection->NativeConvert(&DataStr(m_pEntityInfo->m_strName)), strCurrWorkerID);
			
			if (!nullFilter.IsEmpty())
				nullFilter += _T(" AND ");
			nullFilter += cwsprintf(_T(" ( %s ) "), singleNullFilter);
		}

		if (!joinTxt.IsEmpty() && !nullFilter.IsEmpty() && !existFilter.IsEmpty())
			strFilter = cwsprintf(_T("( ( %s )  %s )"), nullFilter, existFilter);	
	}

	//add the workerID para
	if (pRSWorker)
	{
		CString strParam = cwsprintf(_T("%s_RSWID"), m_pEntityInfo->m_strName);
		if (!pTable->ExistParam(strParam))
			pTable->AddParam(strParam,  *pRSWorker); 
		pTable->SetParamValue(strParam, *pRSWorker); 
	}

	return strFilter;
}

//-----------------------------------------------------------------------------
void RSEntityTableInfo::ValorizeRowSecurityParameters(SqlTable* pTable)
{
	DataObj* pRSWorker = pTable->GetRowSecurityFilterWorker();
	if (pRSWorker)
	{
		CString strParam = cwsprintf(_T("%s_RSWID"), m_pEntityInfo->m_strName);
		if (pTable->ExistParam(strParam))
			pTable->SetParamValue(strParam, *pRSWorker); 
	}
}

//alla select della tabella devo aggiungere anche il campo contenente il tipo di grant su ogni riga estratta che ha il worker attualmente connesso 
//e se non presente nella select anche il campo IsProtected per capire se il record è protetto o meno
//esempio:
//Select ClientCode, CompanyName, RowSecurityID, IsProtected, GrantType =(select grantType from RS_SubjectsGrants where WorkerID = 1 and RowSecurityID = OM_Clients.RowSecurityID) from dbo.OM_Clients
//-----------------------------------------------------------------------------
CString RSEntityTableInfo::GetSelectGrantString(SqlTable* pTable)
{
	if (!pTable || !pTable->GetRecord() || *pTable->GetRecord()->GetNamespace() != m_pEntityInfo->m_MasterTableNamespace)
		return _T("");

	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)pTable->GetRecord()->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	if (!pAddOnFields)
		return _T("");

	DataLng* pRSWorker = pTable->GetRowSecuritySelectWorker();	
	//se il programmatore ha abilitato la possibilità di cambiare il worker allora aggiungo il parametro altrimenti no
	//CString strCurrWorkerID = (pRSWorker) ? _T("?") : pTable->m_pSqlConnection->NativeConvert(&DataLng(AfxGetWorkerId()));

	CString strSelect, strColName;
	BOOL bIsProtectedFound = FALSE;
	for (int n = 0; n <= pTable->m_pColumnArray->GetUpperBound(); n++)
	{
		strColName = pTable->m_pColumnArray->GetAt(n)->GetBindName();
		if (strColName.CompareNoCase(RowSecurityAddOnFields::s_sIsProtected) == 0)
		{
			bIsProtectedFound = TRUE;
			break;
		}
	}
	
	strSelect += cwsprintf(_T("l_CurrentWorkerGrantType = (select GrantType from RS_SubjectsGrants where WorkerID = %s and EntityName = %s and RowSecurityID = %s.RowSecurityID)"),
							pTable->m_pSqlConnection->NativeConvert(&DataLng(AfxGetWorkerId())), pTable->m_pSqlConnection->NativeConvert(&DataStr(m_pEntityInfo->m_strName)), pTable->GetTableName());
	pTable->Select(pTable->GetRecord(), &pAddOnFields->l_CurrentWorkerGrantType, 0); //inserisco in posizione 0 il campo l_CurrentWorkerGrantType per fare la fetch del dato
	if (pRSWorker)
	{
		strSelect += cwsprintf(_T(", l_SpecificWorkerGrantType = (select GrantType from RS_SubjectsGrants where WorkerID = ? and EntityName = %s and RowSecurityID = %s.RowSecurityID)"),
							pTable->m_pSqlConnection->NativeConvert(&DataStr(m_pEntityInfo->m_strName)), pTable->GetTableName());
		pTable->Select(pTable->GetRecord(), &pAddOnFields->l_SpecificWorkerGrantType, 1); //inserisco in posizione 0 il campo l_SpecificWorkerGrantType per fare la fetch del dato
		if (!pTable->ExistParam(_T("SubRSWorkerID")))
			pTable->AddParam(_T("SubRSWorkerID"),  *pRSWorker, SqlParamType::Input, 0); 
		pTable->SetParamValue(_T("SubRSWorkerID"), *pRSWorker); 
	}

	if (!bIsProtectedFound)
	{
		strSelect += cwsprintf(_T(", %s"), RowSecurityAddOnFields::s_sIsProtected);
		pTable->Select(pTable->GetRecord(), &pAddOnFields->f_IsProtected, (pRSWorker) ? 2 : 1); //inserisco in posizione 1 (2 se è stato considerato anche il campo l_SpecificWorkerGrantType)  il campo f_IsProtected per fare la fetch del dato		
	}
	strSelect += _T(", ");
	
	return strSelect;
}

///////////////////////////////////////////////////////////////////////////////
//						RSProtectedTableInfo declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RSProtectedTableInfo::~RSProtectedTableInfo()
{	
}

//-----------------------------------------------------------------------------
BOOL RSProtectedTableInfo::Parse(CXMLNode* pnTable)
{
	CXMLNode* pnEntity = NULL;
	CXMLNodeChildsList* pnEntitiesNode = NULL;

	CString strNamespace;
	pnTable->GetAttribute(_T("namespace"), strNamespace);
	m_TableNamespace.AutoCompleteNamespace(CTBNamespace::TABLE, strNamespace, m_TableNamespace);
	ASSERT(m_TableNamespace.IsValid());		
	m_strTableName = m_TableNamespace.GetObjectName();

	if (m_strTableName.IsEmpty())
		return FALSE;

	if (pnEntitiesNode = pnTable->GetChilds())
	{
		for (int i =0; i < pnEntitiesNode->GetCount(); i++)
		{
			pnEntity = pnEntitiesNode->GetAt(i);
			if (pnEntity)
			{
				RSEntityTableInfo* pEntityTableInfo = new RSEntityTableInfo();
				if (pEntityTableInfo->Parse(pnEntity))
					m_arProtectedInfo.Add((CObject*)pEntityTableInfo);
				else 
					delete pEntityTableInfo;	
			}
		}
	}

	return m_arProtectedInfo.GetCount() > 0;
}

///////////////////////////////////////////////////////////////////////////////
// CRSHierarchyRow definition: identifica una riga della tabella RS_SubjectsHierarchy
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CRSHierarchyRow::CRSHierarchyRow(int aMasterSubjectID, int aSlaveSubjectID, int aNrLevel) 
	: 
	m_MasterSubjectID(aMasterSubjectID),
	m_SlaveSubjectID(aSlaveSubjectID),
	m_NrLevel(aNrLevel),
	m_bVisited(FALSE)
{}

//-----------------------------------------------------------------------------
BOOL CRSHierarchyRow::Match(int nMasterSubjectID, int nSlaveSubjectID, int nLevel)
{
	return 
		(
		m_MasterSubjectID == nMasterSubjectID	&&
		m_SlaveSubjectID == nSlaveSubjectID		&&
		m_NrLevel == nLevel
		);
}

///////////////////////////////////////////////////////////////////////////////
// CRSHierarchyRowArray implementation
///////////////////////////////////////////////////////////////////////////////
//
// aggiunge l'elemento solo se non esiste gia'
//-----------------------------------------------------------------------------
void CRSHierarchyRowArray::Add(int nMasterSubjectID, int nSlaveSubjectID, int nLevel) 
{
	CRSHierarchyRow* pRow = GetElement(nMasterSubjectID, nSlaveSubjectID, nLevel);
	if (pRow)
		return;

	Array::Add(new CRSHierarchyRow(nMasterSubjectID, nSlaveSubjectID, nLevel));
}

//-----------------------------------------------------------------------------
CRSHierarchyRow* CRSHierarchyRowArray::GetElement(int nMasterSubjectID, int nSlaveSubjectID, int nLevel) 
{
	CRSHierarchyRow* pRow = NULL;
	for (int i=0; i<= GetUpperBound(); i++)
	{
		pRow = GetAt(i);
		if (pRow->Match(nMasterSubjectID, nSlaveSubjectID, nLevel))
			return pRow;
	}
	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//							CSubjectCache declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSubjectCache, CObject)

//-----------------------------------------------------------------------------
CSubjectCache::CSubjectCache(TRS_Subjects* pSubjectsRec)
	:
	m_pMasterSubjects	(NULL),
	m_pSlaveSubjects	(NULL),
	m_pResourceElement	(NULL)
{
	m_SubjectID = pSubjectsRec->f_SubjectID;
	m_pResourceElement = new CRSResourceElement(pSubjectsRec->f_WorkerID, pSubjectsRec->f_IsWorker, pSubjectsRec->f_ResourceType, pSubjectsRec->f_ResourceCode, pSubjectsRec->f_Description);	 
}

//-----------------------------------------------------------------------------
CSubjectCache::CSubjectCache(const CSubjectCache& aSubjectCache)
	:
	m_pMasterSubjects	(NULL),
	m_pSlaveSubjects	(NULL)
{
	m_SubjectID = aSubjectCache.m_SubjectID;
	m_pResourceElement = new CRSResourceElement(*aSubjectCache.m_pResourceElement);	
}
	
//-----------------------------------------------------------------------------
CSubjectCache::~CSubjectCache()
{
	if (m_pMasterSubjects)
		delete m_pMasterSubjects;
	if (m_pSlaveSubjects)
		delete m_pSlaveSubjects;
	delete m_pResourceElement;
}

//-----------------------------------------------------------------------------
void CSubjectCache::CopyHierarchyInfo(CSubjectCache* pSourceSubjectCache, CSubjectCacheArray* pSubjectCacheArray)
{
	if (m_pMasterSubjects)
		delete m_pMasterSubjects;
	if (m_pSlaveSubjects)
		delete m_pSlaveSubjects;

	m_pMasterSubjects = new Array();
	m_pSlaveSubjects = new Array();

	CSubjectHierarchy* pSubjectCache = NULL;
	if (pSourceSubjectCache->m_pMasterSubjects)
	{
		for (int i = 0; i <= pSourceSubjectCache->m_pMasterSubjects->GetUpperBound(); i++)
		{
			pSubjectCache = (CSubjectHierarchy*)pSourceSubjectCache->m_pMasterSubjects->GetAt(i);
			m_pMasterSubjects->Add((CObject*)new CSubjectHierarchy(pSubjectCacheArray->GetSubjectCache(pSubjectCache->m_pSubject->m_SubjectID), pSubjectCache->m_nrLevel));				
		}
	}
	if (pSourceSubjectCache->m_pSlaveSubjects)
	{
		for (int i = 0; i <= pSourceSubjectCache->m_pSlaveSubjects->GetUpperBound(); i++)
		{
			pSubjectCache = (CSubjectHierarchy*)pSourceSubjectCache->m_pSlaveSubjects->GetAt(i);
			m_pSlaveSubjects->Add((CObject*)new CSubjectHierarchy(pSubjectCacheArray->GetSubjectCache(pSubjectCache->m_pSubject->m_SubjectID), pSubjectCache->m_nrLevel));				
		}
	}
}

//-----------------------------------------------------------------------------
void CSubjectCache::LoadHierarchyInfo(SqlSession* pSession, CSubjectsManager* pSubjectMng)
{
	//carico le informazioni di organigramma dalla tabella RS_HierarchyExplosion
	TRS_SubjectsHierarchy hierachyRec;
	SqlTable table(&hierachyRec, pSession);
	
	if (m_pMasterSubjects)
		delete m_pMasterSubjects;
	if (m_pSlaveSubjects)
		delete m_pSlaveSubjects;

	m_pMasterSubjects = new Array();
	m_pSlaveSubjects = new Array();

	TRY
	{	
		table.Open(FALSE, E_FAST_FORWARD_ONLY);
		table.m_strSQL = cwsprintf (_T("SELECT MasterSubjectID, SlaveSubjectID, NrLevel FROM %s WHERE MasterSubjectID = %d OR SlaveSubjectID = %d"), 
									TRS_SubjectsHierarchy::GetStaticName(), m_SubjectID, m_SubjectID);
		table.Select(hierachyRec.f_MasterSubjectID);
		table.Select(hierachyRec.f_SlaveSubjectID);		
		table.Select(hierachyRec.f_NrLevel);
		table.Query();

		while(!table.IsEOF())
		{
			if (hierachyRec.f_MasterSubjectID == m_SubjectID)
				m_pSlaveSubjects->Add((CObject*)new CSubjectHierarchy(pSubjectMng->GetSubjectCache(hierachyRec.f_SlaveSubjectID), hierachyRec.f_NrLevel));				
			else
				m_pMasterSubjects->Add((CObject*)new CSubjectHierarchy(pSubjectMng->GetSubjectCache(hierachyRec.f_MasterSubjectID), hierachyRec.f_NrLevel));
			pSubjectMng->SetMaxLevel(hierachyRec.f_NrLevel);
			table.MoveNext();
		}
		table.Close();
	}
	CATCH(SqlException, e)	
	{
		if (table.IsOpen())
			table.Close();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
	}
	END_CATCH
}

///////////////////////////////////////////////////////////////////////////////
//						CSubjectCacheArray declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CSubjectCache* CSubjectCacheArray::GetSubjectCache(int nSubjectID)
{
	CSubjectCache* pSubjectCache = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pSubjectCache = (CSubjectCache*)GetAt(i);
		if (pSubjectCache->m_SubjectID == nSubjectID)
			return pSubjectCache;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CSubjectCache* CSubjectCacheArray::GetSubjectCacheFromWorkerID(int nWorkerID)
{
	CSubjectCache* pSubjectCache = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pSubjectCache = GetAt(i);
		if (pSubjectCache->GetWorkerID() == nWorkerID)
			return pSubjectCache;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CSubjectCache* CSubjectCacheArray::GetSubjectCacheFromResource(const CString& resourceType, const CString& resourceCode)
{
	CSubjectCache* pSubjectCache = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pSubjectCache = GetAt(i);
		if (!pSubjectCache->GetResourceType().CompareNoCase(resourceType) && !pSubjectCache->GetResourceCode().CompareNoCase(resourceCode))
			return pSubjectCache;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
int CSubjectCacheArray::GetSubjectID(int nWorkerID)
{
	CSubjectCache* pSubjectCache = GetSubjectCacheFromWorkerID(nWorkerID);
	return (pSubjectCache) ? pSubjectCache->m_SubjectID : -1;
}

///////////////////////////////////////////////////////////////////////////////
//						CSubjectHierarchy declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSubjectHierarchy, CObject)

//-----------------------------------------------------------------------------
CSubjectHierarchy::CSubjectHierarchy(CSubjectCache* pSubject, short nLevel) 
: 
	m_pSubject(pSubject),
	m_nrLevel(nLevel) 
{}

//-----------------------------------------------------------------------------
CSubjectHierarchy::~CSubjectHierarchy()
{ 	
}

static TCHAR szParamEntityName[]	= _T("EntityName");
static TCHAR szParamRowSecurityID[]	= _T("RowSecurityID");

///////////////////////////////////////////////////////////////////////////////
//             class DBTEntityGrants implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTEntitySubjectsGrants, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTEntitySubjectsGrants::DBTEntitySubjectsGrants
(
    CRuntimeClass*		pClass,
    CAbstractFormDoc*	pDocument,
	const CString&		strEntityName
)
    :
    DBTSlaveBuffered(pClass, pDocument, _NS_DBT("EntitySubjectsGrants"), ALLOW_EMPTY_BODY, TRUE),
	m_EntityName(strEntityName)
{
}

//-----------------------------------------------------------------------------
void DBTEntitySubjectsGrants::OnDefineQuery	()
{
	m_pTable->SelectAll			();

	m_pTable->AddParam			(szParamEntityName,	GetSubjectGrantsRec()->f_EntityName);
	m_pTable->AddFilterColumn	(GetSubjectGrantsRec()->f_EntityName);
	m_pTable->AddParam			(szParamRowSecurityID,	GetSubjectGrantsRec()->f_RowSecurityID);
	m_pTable->AddFilterColumn	(GetSubjectGrantsRec()->f_RowSecurityID);
}
	
//-----------------------------------------------------------------------------
void DBTEntitySubjectsGrants::OnPrepareQuery()
{
	m_pTable->SetParamValue		(szParamEntityName,	m_EntityName);
	DataLng* pRowSecID = (DataLng*)GetMasterRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sRowSecurityID);
	m_pTable->SetParamValue		(szParamRowSecurityID,	*pRowSecID);
}

//-----------------------------------------------------------------------------
void DBTEntitySubjectsGrants::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT (pSqlRec->IsKindOf(RUNTIME_CLASS(TRS_SubjectsGrants)));

	TRS_SubjectsGrants* pRec = (TRS_SubjectsGrants*) pSqlRec;

	pRec->f_EntityName = m_EntityName;
	pRec->f_RowSecurityID = *GetMasterRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sRowSecurityID);
}

//-----------------------------------------------------------------------------
BOOL DBTEntitySubjectsGrants::OnOkTransaction()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DBTEntitySubjectsGrants::FindData(BOOL bPrepareOld /*= TRUE*/)
{
	DBTSlaveBuffered::FindData(bPrepareOld);	
	return TRUE;	
}

// dato un SubjectID ritorna il relativo SqlRecord nel DBT
//----------------------------------------------------------------------------
TRS_SubjectsGrants* DBTEntitySubjectsGrants::GetGrantRecordForSubject(int nSubjectID)
{
	TRS_SubjectsGrants* pRec = NULL;
	for (int i = 0; i <= GetRecords()->GetUpperBound(); i++)
	{
		pRec = (TRS_SubjectsGrants*)GetRecords()->GetAt(i);
		if (pRec->f_SubjectID == nSubjectID)
			return pRec;
	}
	//altrimenti aggiungo un record fittizio con granttype = nogrant	
	pRec = (TRS_SubjectsGrants*)AddRecord();
	pRec->f_SubjectID = nSubjectID;
	pRec->f_GrantType = E_GRANT_TYPE_DENY;
	pRec->f_EntityName = m_EntityName;
	pRec->f_Inherited = FALSE;
	pRec->f_IsImplicit = FALSE;
	pRec->f_RowSecurityID =  *GetMasterRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sRowSecurityID);
	pRec->f_WorkerID = AfxGetSubjectsManager()->GetSubjectCache(nSubjectID)->GetWorkerID();	
	return pRec;
}

//----------------------------------------------------------------------------
void DBTEntitySubjectsGrants::InitSubjectsGrants()
{
	TRS_SubjectsGrants* pRec = NULL;
	
	for (int i = 0; i <= GetRecords()->GetUpperBound(); i++)
	{
		pRec = (TRS_SubjectsGrants*)GetRecords()->GetAt(i);
		if (pRec)
		{
			pRec->f_GrantType = E_GRANT_TYPE_DEFAULT;
			pRec->f_Inherited = FALSE;
			pRec->f_IsImplicit = FALSE;
		}
	}	
}

//----------------------------------------------------------------------------
void DBTEntitySubjectsGrants::AddRemoveImplicitGrants(Array* pSubjectsToGrant, int nOnwerWorkerID, bool bRemove)
{
	CSubjectCache* pSubject = NULL;
	TRS_SubjectsGrants* pDBTSubjectGrant = NULL;

	for (int i=0; i <= pSubjectsToGrant->GetUpperBound(); i++)
	{
		pSubject = (CSubjectCache*)pSubjectsToGrant->GetAt(i);
		if (!pSubject) continue;

		// mi faccio ritornare il record in memoria del DBT corrispondente al subject che sto analizzando
		pDBTSubjectGrant = GetGrantRecordForSubject(pSubject->m_SubjectID);
		if (!pDBTSubjectGrant) continue;

		if (bRemove)
		{
			if (pDBTSubjectGrant->f_IsImplicit) //se è un grant implicito allora lo cancello altrimenti lo mantengo
				pDBTSubjectGrant->f_GrantType = E_GRANT_TYPE_DENY;
		}
		else
		{
			if (pDBTSubjectGrant->f_GrantType != E_GRANT_TYPE_READWRITE)
			{
				pDBTSubjectGrant->f_GrantType = E_GRANT_TYPE_READWRITE;
				pDBTSubjectGrant->f_Inherited = FALSE; // imposto il flag Inheritable a FALSE, visto che e' un grant implicito
				pDBTSubjectGrant->f_IsImplicit = TRUE;
			}
		}
	}
}

// il primo param e' il SqlRecord associato al nodo selezionato nel treeview
// il secondo param e' la lista dei soggetti oggetto di modifica dei grant (in caso di risorsa ci sono anche i figli di primo livello)
// il terzo param e' il granttype scelto dalla combo dall'utente
//---------------------------------------------------------------------------------------------
void DBTEntitySubjectsGrants::ModifyExplicitGrants(TRS_SubjectsGrants* pSubjectsGrantsRec, Array* pSubjectsToGrant, DataEnum grantType)
{
	CSubjectCache* pSubject = NULL;
	TRS_SubjectsGrants* pDBTSubjectGrant = NULL;

	// scorro l'array dei soggetti il cui grant deve essere assegnato/modificato
	for (int i=0; i <= pSubjectsToGrant->GetUpperBound(); i++)
	{
		pSubject = (CSubjectCache*)pSubjectsToGrant->GetAt(i);
		if (!pSubject) continue;

		// mi faccio ritornare il record in memoria del DBT corrispondente al subject che sto analizzando
		pDBTSubjectGrant = GetGrantRecordForSubject(pSubject->m_SubjectID);
		if (!pDBTSubjectGrant) continue;
		
		// se e' un worker e sto analizzando se stesso modifico il grant se:
		// se il granttype e' differente o se è implicito
		// in entrambi i casi metto l'IsInheritable a FALSE
		if	(
				pSubjectsGrantsRec->f_WorkerID != -1 &&  
				pSubjectsGrantsRec->f_SubjectID == pDBTSubjectGrant->f_SubjectID && 
				(pSubjectsGrantsRec->f_GrantType != grantType  || pDBTSubjectGrant->f_IsImplicit) 
			)
		{
			pDBTSubjectGrant->f_GrantType = grantType;
			pDBTSubjectGrant->f_Inherited = FALSE;			
			pDBTSubjectGrant->f_IsImplicit = FALSE;
			continue;
		}

		// se il nodo selezionato e' una risorsa devo skippare se stessa
		if (pSubjectsGrantsRec->f_WorkerID == -1 && pSubjectsGrantsRec->f_SubjectID == pDBTSubjectGrant->f_SubjectID)
		{
			pDBTSubjectGrant->f_GrantType = grantType;
			continue;
		}

		// se il nodo selezionato e' una risorsa scorro i suoi worker figli 
		// se il granttype e' diverso devo impostare il valore al flag IsInheritable (che nasce a false):
		// la prima volta devo metterlo a TRUE, se invece e' gia' a false devo lasciarlo cosi e non toccare il suo valore
		if (
			pSubjectsGrantsRec->f_WorkerID == -1 && 
			pDBTSubjectGrant->f_WorkerID != -1 && 
			pSubjectsGrantsRec->f_GrantType != pDBTSubjectGrant->f_GrantType
			)
		{
			// possiamo pensare che queste condizioni stiano ad indicare la prima volta di assegnazione di un record???
			if (!pDBTSubjectGrant->f_Inherited && pDBTSubjectGrant->f_GrantType == E_GRANT_TYPE_DEFAULT)
			{
				pDBTSubjectGrant->f_GrantType = grantType;
				pDBTSubjectGrant->f_Inherited = TRUE;
				pDBTSubjectGrant->f_IsImplicit = FALSE;
				continue;
			}

			// se il flag e' a true assegno solo il granttype
			if (pDBTSubjectGrant->f_Inherited)
			{
				pDBTSubjectGrant->f_GrantType = grantType;
				continue;
			}

			pDBTSubjectGrant->f_Inherited = FALSE;
		}
	}
}
