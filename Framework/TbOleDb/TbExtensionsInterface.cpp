#include "stdafx.h" 

#include <TbGeneric\GeneralFunctions.h>
#include <TbClientCore\ClientObjects.h>
#include <TbNameSolver\CompanyContext.h>

#include "TbExtensionsInterface.h"

//=========================================================================================
BOOL AFXAPI AfxDataSynchronizeEnabled()
{
	return	AfxIsActivated(TBEXT_APP, _NS_ACT("DataSynchroFunctionality")) &&
		AfxGetLoginInfos()->m_bDataSynchro &&
		AfxGetIDataSynchroManager() &&
		AfxGetIDataSynchroManager()->IsValid() &&
		AfxGetIDataSynchroManager()->ImagoStudioRuntimeInstalled();
}


//=========================================================================================
BOOL AFXAPI AfxSynchroProviderEnabled(const CString& strProviderName)
{
	if (!AfxDataSynchronizeEnabled())
		return FALSE;

	return AfxGetIDataSynchroManager()->IsProviderValid(strProviderName);
}

//la sincronizzazione è relativa all'azienda connessa
//=========================================================================================
IDataSynchroManager* AFXAPI AfxGetIDataSynchroManager()
{
	CCompanyContext* pContext = AfxGetCompanyContext();
	return (IDataSynchroManager*)pContext->GetDataSynchroManager();
}

//=========================================================================================
BOOL AFXAPI AfxRowSecurityEnabled()
{
 return AfxGetLoginInfos()->m_bRowSecurity &&  AfxIsActivated(TBEXT_APP, _NS_ACT("TBRowSecurity"));
}

//=========================================================================================
//	class CRSResourceElement
//=========================================================================================
//-----------------------------------------------------------------------------
CRSResourceElement::CRSResourceElement(int nWorkerID, BOOL bIsWorker, const CString& strResourceType, const CString& strResourceCode, const CString& strDescription)
:
	m_WorkerID(nWorkerID),
	m_IsWorker(bIsWorker),
	m_ResourceType(strResourceType),
	m_ResourceCode(strResourceCode),
	m_Description(strDescription)
{	
}

//-----------------------------------------------------------------------------
CRSResourceElement::CRSResourceElement(const CRSResourceElement& aResourceElem)
{
	m_WorkerID		= aResourceElem.m_WorkerID;		
	m_IsWorker		= aResourceElem.m_IsWorker;		
	m_ResourceType	= aResourceElem.m_ResourceType;
	m_ResourceCode	= aResourceElem.m_ResourceCode;
	m_Description	= aResourceElem.m_Description;
}

////lo valorizzo la prima volta che mi viene chiesto perchè nel costruttore non ho ancora a disposizione la WorkersTable (che viene valorizzata sulla OnDSNChanged del gestionale chiamata dopo le extensions di TB)
////-----------------------------------------------------------------------------
//const CString& CRSResourceElement::GetSubjectTitle()
//{
//	if (m_Title.IsEmpty())
//	{
//		if (m_IsWorker)
//		{
//			CWorker* pWorker = 	AfxGetWorkersTable()->GetWorker(m_WorkerID);
//			m_Title = (pWorker) ? cwsprintf(_T("%s %s"), pWorker->GetName(), pWorker->GetLastName()) : cwsprintf(_T("%d"), m_WorkerID);
//		}
//		else
//			m_Title = cwsprintf(_T("%s %s"), m_ResourceType, m_ResourceCode);
//	}
//	return m_Title;
//}

//=========================================================================================
//	class CRSResourcesArray
//=========================================================================================
// Se esiste l'elemento lo ritorna, altrimenti lo aggiunge
//-----------------------------------------------------------------------------
CRSResourceElement* CRSResourcesArray::GetResource
	(
		int nWorkerID, 
		BOOL bIsWorker, 
		const CString& strResourceType, 
		const CString& strResourceCode
	)
{
	CRSResourceElement* pCurrElem;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pCurrElem = GetAt(i);

		if (pCurrElem->m_IsWorker && bIsWorker)
		{
			if (pCurrElem->m_WorkerID == nWorkerID)
				return pCurrElem;
		}
		else
			if (!pCurrElem->m_IsWorker && !bIsWorker)
			{
				if (
					!pCurrElem->m_ResourceType.CompareNoCase(strResourceType) &&
					!pCurrElem->m_ResourceCode.CompareNoCase(strResourceCode)
					)
					return pCurrElem;
			}
	}

	pCurrElem = new CRSResourceElement(nWorkerID, bIsWorker, strResourceType, strResourceCode, _T(""));
	Add(pCurrElem);
	return pCurrElem;
}

//=========================================================================================
//	class CHierarchyElement
//=========================================================================================
//-----------------------------------------------------------------------------
CHierarchyElement::CHierarchyElement(CRSResourceElement* pMasterElement, CRSResourceElement* pSlaveElement, int nrLevel)
:
	m_pMasterElement(pMasterElement),
	m_pSlaveElement(pSlaveElement),
	m_nrLevel(nrLevel)
{
}

//=========================================================================================
//	class CHierarchyArray
//=========================================================================================
// Verifica se l'elemento passato come parametro e' gia' contenuto nell'array
//-----------------------------------------------------------------------------
BOOL CHierarchyArray::Contains(CHierarchyElement* pElement)
{
	CHierarchyElement* hElem;

	for (int i=0; i <= GetUpperBound(); i++)
	{
		hElem = GetAt(i);

		if (
			MatchElements(pElement->m_pMasterElement, hElem->m_pMasterElement)	&& 
			MatchElements(pElement->m_pSlaveElement, hElem->m_pSlaveElement)		&&
			pElement->m_nrLevel == hElem->m_nrLevel
			)
			return TRUE;
	}

	return FALSE;
}

// esegue il match tra i due parametri (ritorna TRUE se sono uguali)
//-----------------------------------------------------------------------------
BOOL CHierarchyArray::MatchElements(CRSResourceElement* pMasterElement, CRSResourceElement* pHElement)
{
	if (pMasterElement->m_IsWorker != pHElement->m_IsWorker)
		return FALSE;

	if (pMasterElement->m_IsWorker)
	{
		if (pMasterElement->m_WorkerID == pHElement->m_WorkerID)
			return TRUE;
	}
	else
	{
		if (
			pMasterElement->m_ResourceCode == pHElement->m_ResourceCode &&
			pMasterElement->m_ResourceType == pHElement->m_ResourceType
			)
			return TRUE;
	}

	return FALSE;
}

//=========================================================================================
IRowSecurityManager* AFXAPI AfxGetIRowSecurityManager()
{
	CCompanyContext* pContext = AfxGetCompanyContext();
	if (pContext && !pContext->GetRowSecurityManager())
		pContext->AttachRowSecurityManager(new IRowSecurityManager());

	return (IRowSecurityManager*)pContext->GetRowSecurityManager();	
}




//=========================================================================================
IDMSRepositoryManager* AFXAPI AfxGetIDMSRepositoryManager()
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext && !pContext->GetDMSRepositoryManager())
		pContext->AttachDMSRepositoryManager(new IDMSRepositoryManager());

	return (IDMSRepositoryManager*)pContext->GetDMSRepositoryManager();
}